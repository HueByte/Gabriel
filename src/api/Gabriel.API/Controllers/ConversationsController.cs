using System.Text.Json;
using Gabriel.API.Contracts.Conversations;
using Gabriel.API.Contracts.Messages;
using Gabriel.API.Contracts.Sequence;
using Gabriel.API.Mapping;
using Gabriel.Core.Services;
using Gabriel.Engine.Personality;
using Gabriel.Engine.Sequence;
using Gabriel.Engine.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabriel.API.Controllers;

[ApiController]
[Authorize]
[Route("conversations")]
public class ConversationsController : ControllerBase
{
    private static readonly JsonSerializerOptions SseJsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IChatService _chat;
    private readonly IAgentService _agent;
    private readonly IGabrielSequenceService _sequence;
    private readonly IProjectService _projects;
    private readonly PersonalityOptions _personality;

    public ConversationsController(
        IChatService chat,
        IAgentService agent,
        IGabrielSequenceService sequence,
        IProjectService projects,
        IOptions<PersonalityOptions> personality)
    {
        _chat = chat;
        _agent = agent;
        _sequence = sequence;
        _projects = projects;
        _personality = personality.Value;
    }

    // Helper: loads the conversation's parent project (if any) so the response
    // can carry projectIsDefault + effectiveAvatarSeed. Single-conversation
    // endpoints use this; the List endpoint skips it (sidebar rows don't
    // render avatars and the N+1 isn't worth it).
    private async Task<Gabriel.Core.Entities.Project?> LoadProjectAsync(Guid? projectId, CancellationToken ct)
    {
        if (projectId is not { } pid) return null;
        return await _projects.GetAsync(pid, ct);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> List(
        [FromQuery] Guid? projectId,
        CancellationToken ct)
    {
        // projectId is optional: omit it for the "all my conversations" view,
        // pass it to scope to one project.
        var convs = await _chat.ListConversationsAsync(projectId, ct);
        return Ok(convs.Select(c => c.ToResponse(includeMessages: false)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationResponse>> Get(Guid id, CancellationToken ct)
    {
        var conv = await _chat.GetConversationAsync(id, ct);
        var project = await LoadProjectAsync(conv.ProjectId, ct);
        return Ok(conv.ToResponse(includeMessages: true, project: project));
    }

    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> Create(
        [FromBody] CreateConversationRequest request,
        CancellationToken ct)
    {
        var conv = await _chat.CreateConversationAsync(request.ProjectId, request.Title, ct);
        var project = await LoadProjectAsync(conv.ProjectId, ct);
        return CreatedAtAction(nameof(Get), new { id = conv.Id }, conv.ToResponse(includeMessages: true, project: project));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ConversationResponse>> Update(
        Guid id,
        [FromBody] UpdateConversationRequest request,
        CancellationToken ct)
    {
        var conv = await _chat.RenameConversationAsync(id, request.Title, ct);
        var project = await LoadProjectAsync(conv.ProjectId, ct);
        return Ok(conv.ToResponse(includeMessages: false, project: project));
    }

    [HttpPost("{id:guid}/avatar/reroll")]
    public async Task<ActionResult<ConversationResponse>> RerollAvatar(Guid id, CancellationToken ct)
    {
        var conv = await _chat.RerollAvatarAsync(id, ct);
        var project = await LoadProjectAsync(conv.ProjectId, ct);
        return Ok(conv.ToResponse(includeMessages: false, project: project));
    }

    // Pin (or clear) the conversation's avatar skin — meaningful for standalone
    // (Default-project) chats only. Real-project chats render the project's
    // skin, so a pinned conversation-skin is silently ignored at render time
    // (still persisted, so a future "promote chat to project" flow could
    // adopt it). PUT semantics + catalog validation match the project version.
    [HttpPut("{id:guid}/skin")]
    public async Task<ActionResult<ConversationResponse>> SetSkin(
        Guid id,
        [FromBody] Gabriel.API.Contracts.Projects.SetSkinRequest request,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.Pattern) && !SequenceCatalog.IsKnownPattern(request.Pattern))
            return BadRequest(new { detail = $"Unknown pattern '{request.Pattern}'." });
        if (!string.IsNullOrWhiteSpace(request.Palette) && !SequenceCatalog.IsKnownPalette(request.Palette))
            return BadRequest(new { detail = $"Unknown palette '{request.Palette}'." });

        var conv = await _chat.SetSkinAsync(
            id,
            SequenceCatalog.NormalizePattern(request.Pattern),
            SequenceCatalog.NormalizePalette(request.Palette),
            ct);
        var project = await LoadProjectAsync(conv.ProjectId, ct);
        return Ok(conv.ToResponse(includeMessages: false, project: project));
    }

    // Gabriel Sequence — the 64-frame, 16×16 RGB representation of this
    // conversation's personality. Generated server-side from AvatarSeed +
    // ConversationState; not persisted. Cheap to call as often as the client
    // wants a fresh Live State (e.g. once per turn).
    [HttpGet("{id:guid}/sequence")]
    public async Task<ActionResult<GabrielSequenceResponse>> GetSequence(Guid id, CancellationToken ct)
    {
        var sequence = await _sequence.GetForConversationAsync(id, ct);
        return Ok(sequence.ToResponse());
    }

    // Context-window metrics — current tokens, provider window, the threshold
    // at which the next turn would trigger MaybeCompactAsync, and whether any
    // compact has already rolled. Used by the chat UI to draw a usage strip
    // under the avatar; the numbers match the backend's compact decision.
    [HttpGet("{id:guid}/metrics")]
    public async Task<ActionResult<ContextMetricsResponse>> GetMetrics(Guid id, CancellationToken ct)
    {
        var metrics = await _agent.GetContextMetricsAsync(id, ct);
        return Ok(metrics.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _chat.DeleteConversationAsync(id, ct);
        return NoContent();
    }

    // Delete the message AND every message that came after it. Anchors on the
    // earliest sibling in the variant group so a regenerated turn's tail (its
    // tool aftermath + every variant) goes cleanly. Returns the conversation
    // (without messages) so the client can sync UpdatedAt.
    [HttpDelete("{id:guid}/messages/{messageId:guid}")]
    public async Task<ActionResult<ConversationResponse>> DeleteMessage(
        Guid id,
        Guid messageId,
        CancellationToken ct)
    {
        var conv = await _chat.DeleteMessageAsync(id, messageId, ct);
        var project = await LoadProjectAsync(conv.ProjectId, ct);
        return Ok(conv.ToResponse(includeMessages: false, project: project));
    }

    // Switches which variant is active within a variant group. The chosen
    // message becomes active; its siblings become inactive. The client picks
    // which sibling to surface via the variant picker UI.
    [HttpPatch("{id:guid}/messages/{messageId:guid}/active")]
    public async Task<ActionResult<ConversationResponse>> SetActiveVariant(
        Guid id,
        Guid messageId,
        CancellationToken ct)
    {
        var conv = await _chat.SetActiveVariantAsync(id, messageId, ct);
        var project = await LoadProjectAsync(conv.ProjectId, ct);
        return Ok(conv.ToResponse(includeMessages: true, project: project));
    }

    // Regenerate the assistant message at messageId. Same SSE wire format as
    // /messages/stream so the client can reuse its streaming consumer. The new
    // reply shares the original's variantGroupId so the picker UI can navigate
    // between the alternatives.
    [HttpPost("{id:guid}/messages/{messageId:guid}/regenerate")]
    public async Task RegenerateMessage(Guid id, Guid messageId, CancellationToken ct)
    {
        var stream = await _agent.RegenerateAsync(id, messageId, ct);

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        var rng = Random.Shared;
        var thinkingDelayMs = rng.Next(_personality.MinThinkingDelayMs, _personality.MaxThinkingDelayMs + 1);
        var cps = rng.Next(_personality.MinCharsPerSecond, _personality.MaxCharsPerSecond + 1);
        var msPerChar = 1000.0 / cps;
        var firstDeltaSent = false;
        var charsForwarded = 0;
        var streamStartTicks = 0L;

        try
        {
            await foreach (var evt in stream.WithCancellation(ct))
            {
                if (evt is AgentTextDelta td)
                {
                    if (!firstDeltaSent)
                    {
                        await Task.Delay(thinkingDelayMs, ct);
                        streamStartTicks = Environment.TickCount64;
                        firstDeltaSent = true;
                    }
                    else
                    {
                        var elapsedMs = Environment.TickCount64 - streamStartTicks;
                        var targetMs = (long)(charsForwarded * msPerChar);
                        var waitMs = targetMs - elapsedMs;
                        if (waitMs > 0) await Task.Delay((int)waitMs, ct);
                    }
                    charsForwarded += td.Delta.Length;
                }

                var json = JsonSerializer.Serialize<AgentEvent>(evt, SseJsonOpts);
                await Response.WriteAsync($"data: {json}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            var fallback = JsonSerializer.Serialize<AgentEvent>(new AgentError(ex.Message), SseJsonOpts);
            await Response.WriteAsync($"data: {fallback}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }

    // SSE endpoint — yields AgentEvent JSON frames (textDelta, toolCall, toolResult,
    // assistantMessage, done, error). Each frame is a single `data: ...\n\n` line.
    // Pre-flight validation (empty input, missing conversation) throws before any
    // bytes are written so 4xx/ProblemDetails responses still work.
    //
    // Typing-tempo simulation lives here (Personality phase 1): a fixed "thinking"
    // delay before the first textDelta and a per-character throttle on subsequent
    // ones, so the wire pace looks like a human typing instead of bursty provider
    // chunks. Other event types (toolCall / toolResult / assistantMessage / done /
    // error) bypass the throttle.
    [HttpPost("{id:guid}/messages/stream")]
    public async Task StreamMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken ct)
    {
        // AgentService.RunAsync awaits the conversation load BEFORE returning the
        // iterator, so a NotFoundException here is raised pre-stream and the
        // global handler can convert it to a clean 404.
        var stream = await _agent.RunAsync(id, request.Content, ct);

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no"; // bypass reverse-proxy buffering

        var rng = Random.Shared;
        var thinkingDelayMs = rng.Next(_personality.MinThinkingDelayMs, _personality.MaxThinkingDelayMs + 1);
        var cps = rng.Next(_personality.MinCharsPerSecond, _personality.MaxCharsPerSecond + 1);
        var msPerChar = 1000.0 / cps;
        var firstDeltaSent = false;
        var charsForwarded = 0;
        var streamStartTicks = 0L;

        try
        {
            await foreach (var evt in stream.WithCancellation(ct))
            {
                if (evt is AgentTextDelta td)
                {
                    if (!firstDeltaSent)
                    {
                        // "Thinking" pause before the first character lands.
                        await Task.Delay(thinkingDelayMs, ct);
                        streamStartTicks = Environment.TickCount64;
                        firstDeltaSent = true;
                    }
                    else
                    {
                        // Throttle to cps: target time = chars * msPerChar from stream
                        // start. Sleep if we're ahead, ship immediately if behind.
                        var elapsedMs = Environment.TickCount64 - streamStartTicks;
                        var targetMs = (long)(charsForwarded * msPerChar);
                        var waitMs = targetMs - elapsedMs;
                        if (waitMs > 0) await Task.Delay((int)waitMs, ct);
                    }
                    charsForwarded += td.Delta.Length;
                }

                var json = JsonSerializer.Serialize<AgentEvent>(evt, SseJsonOpts);
                await Response.WriteAsync($"data: {json}\n\n", ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Client disconnected mid-stream — nothing to send back.
        }
        catch (Exception ex)
        {
            // Headers already sent; surface as a final SSE error event instead of a 500.
            var fallback = JsonSerializer.Serialize<AgentEvent>(new AgentError(ex.Message), SseJsonOpts);
            await Response.WriteAsync($"data: {fallback}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}
