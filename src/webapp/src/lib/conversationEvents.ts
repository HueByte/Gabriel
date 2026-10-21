// In-app broadcast for conversation metadata that changes mid-stream. Chat
// receives these over SSE; the Sidebar owns the conversation list. A window
// event decouples the two without lifting the list into shared state (same
// pattern as PREFS_CHANGED_EVENT in userPrefs.ts).

const CONVERSATION_TITLED_EVENT = 'gabriel:conversation-titled';

interface ConversationTitledDetail {
  conversationId: string;
  title: string;
}

export function notifyConversationTitled(conversationId: string, title: string): void {
  window.dispatchEvent(new CustomEvent<ConversationTitledDetail>(CONVERSATION_TITLED_EVENT, {
    detail: { conversationId, title },
  }));
}

// Returns the unsubscribe function, so it drops straight into a useEffect.
export function onConversationTitled(handler: (conversationId: string, title: string) => void): () => void {
  const listener = (e: Event) => {
    const { conversationId, title } = (e as CustomEvent<ConversationTitledDetail>).detail;
    handler(conversationId, title);
  };
  window.addEventListener(CONVERSATION_TITLED_EVENT, listener);
  return () => window.removeEventListener(CONVERSATION_TITLED_EVENT, listener);
}
