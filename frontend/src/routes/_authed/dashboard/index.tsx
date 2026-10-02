import { createFileRoute } from '@tanstack/react-router';
import { useEffect, useState } from 'react';

export const Route = createFileRoute('/_authed/dashboard/')({
  component: DashboardPage,
});

interface ShortUrl {
  id: string;
  url: string;
  shortCode?: string;
}

function DashboardPage() {
  const [shortUrls, setShortUrls] = useState<ShortUrl[]>([]);

  useEffect(() => {
    const getUrls = async () => {
      try {
        const res: Response = await fetch('/api/shorturls', {
          method: 'GET',
          credentials: 'same-origin',
        });
        const data: ShortUrl[] = (await res.json()) as ShortUrl[];
        setShortUrls(data);
      } catch (error) {
        console.error(error);
      }
    };
    void getUrls();
  }, []);

  return (
    <div>
      <h1>Dashboard</h1>
      <ul>
        {shortUrls.map((s) => (
          <li key={s.id}>{s.url}</li>
        ))}
      </ul>
    </div>
  );
}
