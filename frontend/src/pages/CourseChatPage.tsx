import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import MessageContent from '../components/MessageContent';
import api from '../services/api';
import type { Course } from '../services/knowledgeService';
import type { ChatMessage } from '../services/chatService';

interface ChatResponse {
  message: string;
  toolCalls?: any[];
  requiresAction: boolean;
  toolResults?: any[];
}

export default function CourseChatPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [course, setCourse] = useState<Course | null>(null);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    loadCourseAndHistory();
  }, [id]);

  useEffect(() => {
    scrollToBottom();
  }, [messages]);

  const loadCourseAndHistory = async () => {
    if (!id) return;

    try {
      setInitialLoading(true);

      // Load course details
      const courseResponse = await api.get<Course>(`/knowledge/courses/${id}`);
      setCourse(courseResponse.data);

      // Load course chat history
      const historyResponse = await api.get<ChatMessage[]>(`/chat/course/${id}/history`);
      setMessages(historyResponse.data);
    } catch (err) {
      console.error('Failed to load course data:', err);
    } finally {
      setInitialLoading(false);
    }
  };

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleSendMessage = async (message: string) => {
    if (!message.trim() || loading || !id) return;

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
      const response = await api.post<ChatResponse>(`/chat/course/${id}/message`, {
        message: message,
      });

      // Add assistant message
      const assistantMsg: ChatMessage = {
        id: Date.now() + 1,
        userId: user?.id || '',
        role: 'assistant',
        content: response.data.message,
        toolCalls: response.data.toolCalls ? JSON.stringify(response.data.toolCalls) : undefined,
        timestamp: new Date().toISOString(),
      };

      setMessages((prev) => [...prev, assistantMsg]);

      // If there are tool results, show them
      if (response.data.toolResults && response.data.toolResults.length > 0) {
        const successResults = response.data.toolResults.filter((r: any) => r.success);
        if (successResults.length > 0) {
          const toolResultsMsg: ChatMessage = {
            id: Date.now() + 2,
            userId: user?.id || '',
            role: 'system',
            content: successResults.map((r: any) => `✓ ${r.result}`).join('\n'),
            timestamp: new Date().toISOString(),
          };
          setMessages((prev) => [...prev, toolResultsMsg]);
        }
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

  const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
  };

  if (initialLoading) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="text-gray-500">Loading course...</div>
      </div>
    );
  }

  if (!course) {
    return (
      <div className="p-6">
        <div className="text-center text-gray-500 py-12">
          <p className="text-lg">Course not found</p>
          <button
            onClick={() => navigate('/skills')}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            Back to Skills
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full">
      {/* Course Header */}
      <div className="bg-gradient-to-r from-green-600 to-green-500 text-white px-6 py-4 shadow-lg">
        <button
          onClick={() => navigate(`/course/${id}`)}
          className="text-white/80 hover:text-white mb-2 text-sm flex items-center gap-1"
        >
          ← Back to Course Overview
        </button>
        <h1 className="text-2xl font-bold">{course.name}</h1>
        <p className="text-green-100 text-sm mt-1">Interactive Learning Session</p>
      </div>

      {/* Chat Container */}
      <div className="flex-1 overflow-y-auto p-6 space-y-4 bg-gray-50">
        {messages.length === 0 && (
          <div className="flex justify-start mb-4">
            <div className="max-w-[80%] rounded-lg px-6 py-4 bg-gradient-to-r from-green-100 to-green-50 border-2 border-green-200">
              <div className="text-3xl mb-3">👨‍🏫 Welcome to Interactive Learning!</div>
              <p className="text-gray-800 mb-3">
                I'm your programming teacher for <strong>{course.name}</strong>. I'll teach you through:
              </p>
              <ol className="list-decimal list-inside space-y-2 text-gray-700 mb-4">
                <li>Clear explanations with real code examples</li>
                <li>Interactive code blocks you can edit and run</li>
                <li>Step-by-step breakdowns of complex concepts</li>
                <li>Comprehension questions to test your understanding</li>
                <li>Practice exercises to reinforce learning</li>
              </ol>
              <p className="text-green-700 font-medium">
                Ready to start? Ask me anything about {course.name}, or type "Let's begin!" to start the first lesson.
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
              className={`max-w-[80%] rounded-lg px-4 py-3 ${
                msg.role === 'user'
                  ? 'bg-blue-600 text-white'
                  : msg.role === 'system'
                  ? 'bg-gray-100 text-gray-700 text-sm'
                  : 'bg-white text-gray-900 shadow-md border border-gray-200'
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
            <div className="bg-white shadow-md rounded-lg px-4 py-3 border border-gray-200">
              <div className="flex items-center gap-2">
                <div className="w-2 h-2 bg-green-500 rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
                <div className="w-2 h-2 bg-green-500 rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
                <div className="w-2 h-2 bg-green-500 rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
              </div>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* Input Area */}
      <div className="border-t border-gray-300 bg-white p-4 shadow-lg">
        <form onSubmit={handleSend} className="flex gap-3">
          <input
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Ask a question or request an exercise..."
            className="flex-1 px-4 py-3 border-2 border-green-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-green-500 focus:border-transparent"
            disabled={loading}
          />
          <button
            type="submit"
            disabled={loading || !input.trim()}
            className="px-8 py-3 bg-gradient-to-r from-green-600 to-green-500 text-white font-medium rounded-lg hover:from-green-700 hover:to-green-600 focus:outline-none focus:ring-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-md hover:shadow-lg"
          >
            Send
          </button>
        </form>
        <p className="text-xs text-gray-500 mt-2">
          💡 Try code blocks with syntax highlighting! They're editable and runnable.
        </p>
      </div>
    </div>
  );
}
