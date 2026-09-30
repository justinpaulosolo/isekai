import { createFileRoute, useNavigate } from '@tanstack/react-router';
import { useState } from 'react';

export const Route = createFileRoute('/')({
  component: Index,
});

function Index() {
  const [longUrl, setLongUrl] = useState<string>('https://www.shorturl.at/');
  const navigate = useNavigate();

  const onShortenClick = async () => {
    const payload = {
      url: longUrl,
    };

    try {
      const res = await fetch(
        'https://server-isekai.dev.localhost:7342/api/shorturls',
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(payload),
        },
      );

      if (!res.ok) {
        throw new Error(`HTTP error! Status: ${res.status}`);
      }

      const result = await res.json();
      // console.log('Success:', result);

      navigate({ to: '/shortener', state: { ...result } });
    } catch (error) {
      console.error(error);
    }

    //Need validation for
  };

  return (
    <div className="w-2/5 mx-auto text-center mt-10">
      {/*  */}

      <p className="text-sky-500 font-mono">
        Modern & Blazing Fast Link Shortener
      </p>

      <h1 className="text-6xl font-bold font-jakarta">
        Short links, boundless reach.
      </h1>

      <p>
        Fast, secure, and modern link shortening with real-time analytics and
        instant edge routing.
      </p>

      {/* Need to change to url */}
      <input
        type="url"
        placeholder="https://example.com"
        className="input input-primary"
        name="longUrl"
        value={longUrl}
        onChange={(e) => setLongUrl(e.currentTarget.value)}
      />
      <button className="btn btn-outline btn-primary" onClick={onShortenClick}>
        Shorten URL
      </button>

      <p>
        URL shortener allows to create a shortened link making it easy to share
      </p>
    </div>
  );
}
