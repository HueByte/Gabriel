using System.Text.Json;
using Gabriel.API.Contracts.Conversations;
using Gabriel.API.Contracts.Messages;
using Gabriel.API.Mapping;
using Gabriel.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gabriel.API.Controllers;

[ApiController]
[Authorize]
[Route("conversations")]
public class ConversationsController : ControllerBase
{
    private static readonly JsonSerializerOptions SseJsonOpts = new(JsonSerializerDefaults.Web);

    private readonly IChatService _chat;
    private readonly IAgentService _agent;

    public ConversationsController(IChatService chat, IAgentService agent)
    {
        _chat = chat;
        _agent = agent;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationResponse>>> List(CancellationToken ct)
    {
        var convs = await _chat.ListConversationsAsync(ct);
        return Ok(convs.Select(c => c.ToResponse(includeMessages: false)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationResponse>> Get(Guid id, CancellationToken ct)
    {
        var conv = await _chat.GetConversationAsync(id, ct);
        return Ok(conv.ToResponse(includeMessages: true));
    }

    [HttpPost]
    public async Task<ActionResult<ConversationResponse>> Create(
        [FromBody] CreateConversationRequest request,
        CancellationToken ct)
    {
        var conv = await _chat.CreateConversationAsync(request.Title, ct);
        return CreatedAtAction(nameof(Get), new { id = conv.Id }, conv.ToResponse(includeMessages: true));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<ConversationResponse>> Update(
        Guid id,
        [FromBody] UpdateConversationRequest request,
        CancellationToken ct)
    {
        var conv = await _chat.RenameConversationAsync(id, request.Title, ct);
        return Ok(conv.ToResponse(includeMessages: false));
    }

    [HttpPost("{id:guid}/avatar/reroll")]
    public async Task<ActionResult<ConversationResponse>> RerollAvatar(Guid id, CancellationToken ct)
    {
        var conv = await _chat.RerollAvatarAsync(id, ct);
        return Ok(conv.ToResponse(includeMessages: false));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _chat.DeleteConversationAsync(id, ct);
        return NoContent();
    }

    // SSE endpoint — yields AgentEvent JSON frames (textDelta, toolCall, toolResult,
    // assistantMessage, done, error). Each frame is a single `data: ...\n\n` line.
    // Pre-flight validation (empty input, missing conversation) throws before any
    // bytes are written so 4xx/ProblemDetails responses still work.
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

        try
        {
            await foreach (var evt in stream.WithCancellation(ct))
            {
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
