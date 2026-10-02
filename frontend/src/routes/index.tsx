import { createFileRoute, useNavigate } from '@tanstack/react-router';
import { useState } from 'react';
import {
  Link2,
  Scissors,
  Shield,
  ChartNoAxesCombined,
  Zap,
} from 'lucide-react';

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
      const res = await fetch('/api/shorturls', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload),
      });

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
    <div className="w-4/5 lg:w-2/5 mx-auto text-center mt-10">
      <div className="inline-flex items-center gap-2 px-3 py-1 bg-base-300 text-sky-400  border border-slate-50/20 rounded-full backdrop-blur-sm font-mono">
        <Link2 className="w-3.5 h-3.5 text-sky-400" />
        Modern &amp; Blazing Fast Link Shortener
      </div>

      <h1 className="text-6xl font-bold font-jakarta mt-5">
        Short links,{' '}
        <span className="bg-linear-to-r from-indigo-300 via-sky-400 to-indigo-300 bg-clip-text text-transparent">
          boundless reach
        </span>
        .
      </h1>

      <p className="text-xl text-slate-400 mt-5">
        Fast, secure, and modern link shortening with real-time analytics and
        instant edge routing.
      </p>

      {/* Need to change to url */}
      <div className="bg-base-200 border border-zinc-800/80 rounded-sm p-3 mt-10 lg:flex">
        <label className="input w-full bg-base-400 border-slate-700 outline-none focus-within:outline-none focus-within:ring-0 focus-within:bg-base-300 transition-colors">
          <Link2 className="w-3.5 h-3.5 text-slate-400 shrink-0" />
          <input
            type="url"
            placeholder="https://example.com"
            className="grow focus:outline-none bg-transparent font-mono"
            name="longUrl"
            value={longUrl}
            onChange={(e) => setLongUrl(e.currentTarget.value)}
          />
        </label>

        <button
          className="btn bg-indigo-500 hover:bg-indigo-300 text-indigo-950 ms-1 p-3 mt-2 lg:mt-0"
          onClick={onShortenClick}
        >
          <Scissors />
          Shorten URL
        </button>
      </div>

      {/* Cards */}
      <div className="bg-base-200 border border-zinc-800/80 rounded-sm p-3 mt-5 lg:flex gap-4 justify-center">
        <div className="flex items-center gap-3 px-4 py-3 rounded-xl border border-white/8 bg-white/2 lg:mt-0 mt-2">
          <div className="flex items-center justify-center w-8 h-8 rounded-lg bg-indigo-500/10 text-sky-400">
            <Zap className="w-4 h-4" />
          </div>

          <div>
            <h3 className="text-sm font-semibold text-slate-100 leading-5 text-left">
              &lt;12ms Edge Redirects
            </h3>
            <p className="text-xs font-normal text-slate-400 leading-4 text-left">
              Global Anycast routing
            </p>
          </div>
        </div>

        <div className="flex items-center gap-3 px-4 py-3 rounded-xl border border-white/8 bg-white/2 lg:mt-0 mt-2">
          <div className="flex items-center justify-center w-8 h-8 rounded-lg bg-indigo-500/10 text-indigo-400">
            <Shield className="w-4 h-4" />
          </div>
          <div className="">
            <h3 className="text-sm font-semibold text-slate-100 leading-5 text-left">
              Free SSL & Bot Shield
            </h3>
            <p className="text-xs font-normal text-slate-400 leading-4 text-left">
              Automated threat defense
            </p>
          </div>
        </div>

        <div className="flex items-center gap-3 px-4 py-3 rounded-xl border border-white/8 bg-white/2 lg:mt-0 mt-2">
          <div className="flex items-center justify-center w-8 h-8 rounded-lg bg-indigo-500/10 text-sky-400">
            <ChartNoAxesCombined className="w-4 h-4" />
          </div>
          <div>
            <h3 className="text-sm font-semibold text-slate-100 leading-5 text-left">
              Live Telemetry
            </h3>
            <p className="text-xs font-normal text-slate-400 leading-4">
              Zero-latency click analytics
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
