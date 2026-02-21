import { useNavigate } from 'react-router-dom';
import ReactMarkdown from 'react-markdown';
import CodeBlock from './CodeBlock';

interface MessageContentProps {
  content: string;
  role: 'user' | 'assistant' | 'system';
}

export default function MessageContent({ content, role }: MessageContentProps) {
  const navigate = useNavigate();

  if (role !== 'assistant') {
    return <div className="whitespace-pre-wrap break-words">{content}</div>;
  }

  // Extract YouTube video ID from URL
  const extractYouTubeId = (url: string): string | null => {
    const patterns = [
      /(?:youtube\.com\/watch\?v=|youtu\.be\/)([a-zA-Z0-9_-]{11})/,
      /youtube\.com\/embed\/([a-zA-Z0-9_-]{11})/
    ];

    for (const pattern of patterns) {
      const match = url.match(pattern);
      if (match) return match[1];
    }
    return null;
  };

  // Custom renderer for markdown elements
  const components = {
    // Interactive code blocks with Monaco Editor
    code: ({ node, inline, className, children, ...props }: any) => {
      const match = /language-(\w+)/.exec(className || '');
      const language = match ? match[1] : '';
      const codeString = String(children).replace(/\n$/, '');

      return !inline && language ? (
        <CodeBlock code={codeString} language={language} />
      ) : (
        <code className="bg-green-100 text-green-800 px-1.5 py-0.5 rounded text-sm font-mono" {...props}>
          {children}
        </code>
      );
    },
    // Make bold text potentially clickable if it's a skill/topic/course
    strong: ({ children, ...props }: any) => {
      const text = children?.toString() || '';

      // Detect if this might be a skill, topic, or course name
      // Heuristic: if it's quoted or contains common course keywords
      const isCourseName = text.includes('"') || text.toLowerCase().includes('course') ||
                          text.toLowerCase().includes('beginner') || text.toLowerCase().includes('tutorial');

      if (isCourseName) {
        return (
          <strong
            {...props}
            className="cursor-pointer hover:text-blue-700 hover:underline"
            onClick={(e) => {
              e.stopPropagation();
              // For now, navigate to skills page
              // TODO: Implement actual skill/topic/course lookup and navigation
              navigate('/skills');
            }}
          >
            {children}
          </strong>
        );
      }

      return <strong {...props}>{children}</strong>;
    },
    // Custom link renderer - convert YouTube links to embedded videos
    a: ({ href, children, ...props }: any) => {
      // Check if this is a YouTube link
      const videoId = href ? extractYouTubeId(href) : null;

      if (videoId) {
        return (
          <div className="my-4">
            <div className="aspect-video w-full max-w-2xl">
              <iframe
                src={`https://www.youtube.com/embed/${videoId}`}
                title="YouTube video"
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                allowFullScreen
                className="w-full h-full rounded-lg border-2 border-gray-200"
              />
            </div>
            <p className="text-sm text-gray-600 mt-2">{children}</p>
          </div>
        );
      }

      // Regular links
      return (
        <a
          {...props}
          href={href}
          className="text-blue-600 hover:text-blue-700 hover:underline"
          target="_blank"
          rel="noopener noreferrer"
        >
          {children}
        </a>
      );
    },
  };

  return (
    <div className="prose prose-sm max-w-none [&_p]:!my-5 [&_p]:leading-relaxed [&_h1]:!mt-8 [&_h1]:!mb-6 [&_h2]:!mt-8 [&_h2]:!mb-6 [&_h3]:!mt-6 [&_h3]:!mb-4 [&_ul]:!my-5 [&_ol]:!my-5 [&_li]:my-2">
      <ReactMarkdown components={components}>{content}</ReactMarkdown>
    </div>
  );
}
