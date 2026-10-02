import { createFileRoute, useNavigate, useRouterState } from '@tanstack/react-router';
import { useEffect } from 'react';

export const Route = createFileRoute('/shortener')({
  component: Shortener,
});

function Shortener() {
  const navigate = useNavigate();
  const apiData = useRouterState({
    select: (state) => state.location.state,
  });

  useEffect(() => {
    if (!apiData) {
      void navigate({ to: '/' });
    }
  }, [apiData, navigate]);

  return (
    <>
      <h1>Your shortened URL </h1>
      <p>
        Copy the short link and share it in messages, texts, posts, websites and other locations.
      </p>
    </>
  );
}
