import { useState, useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import { chatService } from '../services/chatService';
import type { ChatMessage, ToolResult } from '../services/chatService';
import { useAuthStore } from '../stores/authStore';
import MessageContent from './MessageContent';

interface ChatInterfaceProps {
  scrollToTop?: boolean;
  onScrollComplete?: () => void;
}

export default function ChatInterface({ scrollToTop = false, onScrollComplete }: ChatInterfaceProps = {}) {
  const { user } = useAuthStore();
  const location = useLocation();
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [isListening, setIsListening] = useState(false);
  const [ttsEnabled, setTtsEnabled] = useState(() => {
    const saved = localStorage.getItem('ttsEnabled');
    return saved ? JSON.parse(saved) : false;
  });
  const messagesStartRef = useRef<HTMLDivElement>(null);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const recognitionRef = useRef<any>(null);
  const courseStartTriggered = useRef(false);

  useEffect(() => {
    loadHistory();
  }, []);

  useEffect(() => {
    if (scrollToTop) {
      // Scroll to top for course start
      messagesStartRef.current?.scrollIntoView({ behavior: 'smooth' });
      onScrollComplete?.();
    } else {
      // Normal scroll to bottom for new messages
      scrollToBottom();
    }
  }, [messages, scrollToTop, onScrollComplete]);

  useEffect(() => {
    // Handle course start from navigation
    const state = location.state as any;
    if (state?.startCourse && state?.courseId && !courseStartTriggered.current && !initialLoading) {
      courseStartTriggered.current = true;
      const startMessage = `Start teaching me the course: ${state.courseName}. Show me an introduction video.`;

      // Use setTimeout to ensure chat history is loaded first
      setTimeout(() => {
        handleSendMessage(startMessage);
      }, 500);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.state, initialLoading]);

  const loadHistory = async () => {
    try {
      const history = await chatService.getHistory();
      setMessages(history);
    } catch (err) {
      console.error('Failed to load chat history:', err);
    } finally {
      setInitialLoading(false);
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = async (message: string) => {
    if (!message.trim() || loading) return;

    setLoading(true);

    // Add user message to UI immediately
    const tempUserMsg: ChatMessage = {
      id: Date.now(),
      userId: user?.id || '',
      role: 'user',
      content: message,
      timestamp: new Date().toISOString(),
    };
    setMessages((prev) => [...prev, tempUserMsg]);

    try {
      const response = await chatService.sendMessage(message);

      // Add assistant message
      const assistantMsg: ChatMessage = {
        id: Date.now() + 1,
        userId: user?.id || '',
        role: 'assistant',
        content: response.message,
        toolCalls: response.toolCalls ? JSON.stringify(response.toolCalls) : undefined,
        timestamp: new Date().toISOString(),
      };

      setMessages((prev) => [...prev, assistantMsg]);

      // Speak the assistant's response if TTS is enabled
      speakText(response.message);

      // If there are tool results, show them
      if (response.toolResults && response.toolResults.length > 0) {
        const toolResultsMsg: ChatMessage = {
          id: Date.now() + 2,
          userId: user?.id || '',
          role: 'system',
          content: formatToolResults(response.toolResults),
          timestamp: new Date().toISOString(),
        };
        setMessages((prev) => [...prev, toolResultsMsg]);
      }
    } catch (err) {
      console.error('Failed to send message:', err);
      const errorMsg: ChatMessage = {
        id: Date.now() + 1,
        userId: user?.id || '',
        role: 'system',
        content: 'Sorry, I encountered an error. Please try again.',
        timestamp: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, errorMsg]);
    } finally {
      setLoading(false);
    }
  };

  const handleSend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || loading) return;

    const userMessage = input.trim();
    setInput('');
    await handleSendMessage(userMessage);
  };

  const formatToolResults = (results: ToolResult[]): string => {
    // Filter out error messages (Unknown tool, etc.)
    const successfulResults = results.filter(r => r.success);
    if (successfulResults.length === 0) return '';

    return successfulResults
      .map((r) => `✓ ${r.result}`)
      .join('\n');
  };

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  };

  const startListening = () => {
    // Check if browser supports speech recognition
    const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;

    if (!SpeechRecognition) {
      alert('Speech recognition is not supported in your browser. Please use Chrome, Edge, or Safari.');
      return;
    }

    if (!recognitionRef.current) {
      const recognition = new SpeechRecognition();
      recognition.continuous = false;
      recognition.interimResults = false;
      recognition.lang = 'en-US';

      recognition.onstart = () => {
        setIsListening(true);
      };

      recognition.onresult = (event: any) => {
        const transcript = event.results[0][0].transcript;
        setInput(transcript);
      };

      recognition.onerror = (event: any) => {
        console.error('Speech recognition error:', event.error);
        setIsListening(false);
      };

      recognition.onend = () => {
        setIsListening(false);
      };

      recognitionRef.current = recognition;
    }

    recognitionRef.current.start();
  };

  const stopListening = () => {
    if (recognitionRef.current) {
      recognitionRef.current.stop();
      setIsListening(false);
    }
  };

  const speakText = (text: string) => {
    if (!ttsEnabled) return;

    // Cancel any ongoing speech
    window.speechSynthesis.cancel();

    // Remove markdown formatting for better speech
    const cleanText = text
      .replace(/\*\*(.+?)\*\*/g, '$1') // Remove bold
      .replace(/\*(.+?)\*/g, '$1') // Remove italic
      .replace(/`(.+?)`/g, '$1') // Remove code
      .replace(/\[(.+?)\]\(.+?\)/g, '$1') // Remove links but keep text
      .replace(/#+\s/g, '') // Remove heading markers
      .replace(/```[\s\S]+?```/g, 'code example') // Replace code blocks
      .trim();

    const utterance = new SpeechSynthesisUtterance(cleanText);
    utterance.rate = 0.9; // Slightly slower for clarity
    utterance.pitch = 1.0;
    utterance.volume = 1.0;

    window.speechSynthesis.speak(utterance);
  };

  const toggleTts = () => {
    const newValue = !ttsEnabled;
    setTtsEnabled(newValue);
    localStorage.setItem('ttsEnabled', JSON.stringify(newValue));

    // Stop any ongoing speech when disabling
    if (!newValue) {
      window.speechSynthesis.cancel();
    }
  };

  if (initialLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-gray-500">Loading chat...</div>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-white rounded-lg shadow">
      {/* Messages */}
      <div className="flex-1 overflow-y-auto p-6 space-y-4">
        <div ref={messagesStartRef} />
        {messages.length === 0 && (
          <div className="flex justify-start mb-4">
            <div className="max-w-[70%] rounded-lg px-4 py-3 bg-gradient-to-r from-green-100 to-green-50 border-2 border-green-200">
              <div className="text-3xl mb-3">👋 Welcome to Prospergenics Learning!</div>
              <p className="text-gray-800 mb-2">
                I'm here to help you learn technology skills step by step. Here's how we'll work together:
              </p>
              <ol className="list-decimal list-inside space-y-1 text-gray-700 mb-3">
                <li>Tell me what you want to learn</li>
                <li>I'll ask what you already know</li>
                <li>I'll create a personalized learning path for you</li>
                <li>We'll start with topics and courses that match your level</li>
              </ol>
              <p className="text-green-700 font-medium">
                What would you like to learn today? (Examples: AI, Web Development, Python, etc.)
              </p>
            </div>
          </div>
        )}

        {messages.map((msg) => (
          <div
            key={msg.id}
            className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
          >
            <div
              className={`max-w-[70%] rounded-lg px-4 py-3 ${
                msg.role === 'user'
                  ? 'bg-blue-600 text-white'
                  : msg.role === 'system'
                  ? 'bg-gray-100 text-gray-700 text-sm'
                  : 'bg-gray-200 text-gray-900'
              }`}
            >
              <MessageContent content={msg.content} role={msg.role} />
              {msg.toolCalls && (
                <div className="mt-2 pt-2 border-t border-blue-400 text-xs opacity-80">
                  🔧 Tool called: {JSON.parse(msg.toolCalls).length} action(s)
                </div>
              )}
              <div
                className={`text-xs mt-1 ${
                  msg.role === 'user' ? 'text-blue-100' : 'text-gray-500'
                }`}
              >
                {formatTimestamp(msg.timestamp)}
              </div>
            </div>
          </div>
        ))}

        {loading && (
          <div className="flex justify-start">
            <div className="bg-gray-200 rounded-lg px-4 py-3">
              <div className="flex items-center gap-2">
                <div className="w-2 h-2 bg-gray-500 rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
                <div className="w-2 h-2 bg-gray-500 rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
                <div className="w-2 h-2 bg-gray-500 rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
              </div>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* Input */}
      <div className="border-t border-gray-200 p-4">
        <form onSubmit={handleSend} className="flex gap-3">
          <input
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Ask me about skills you want to learn..."
            className="flex-1 px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={loading}
          />
          <button
            type="button"
            onClick={toggleTts}
            className={`px-4 py-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500 transition-all ${
              ttsEnabled
                ? 'bg-gradient-to-r from-green-600 to-green-500 text-white hover:shadow-lg'
                : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
            }`}
            title={ttsEnabled ? 'Text-to-Speech enabled (click to disable)' : 'Enable Text-to-Speech'}
          >
            {ttsEnabled ? '🔊' : '🔇'}
          </button>
          <button
            type="button"
            onClick={isListening ? stopListening : startListening}
            disabled={loading}
            className={`px-4 py-3 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed ${
              isListening
                ? 'bg-red-600 text-white hover:bg-red-700 animate-pulse'
                : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
            }`}
            title={isListening ? 'Stop listening' : 'Start voice input'}
          >
            {isListening ? '⏹' : '🎤'}
          </button>
          <button
            type="submit"
            disabled={loading || !input.trim()}
            className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Send
          </button>
        </form>
      </div>
    </div>
  );
}
