import {
  createFileRoute,
  useRouterState,
  useNavigate,
} from '@tanstack/react-router';

export const Route = createFileRoute('/shortener')({
  component: Shortener,
});

function Shortener() {
  const navigate = useNavigate();
  const apiData = useRouterState({
    select: (state) => state.location.state,
  });

  if (!apiData) {
    navigate({ to: '/' });
  }

  console.log(apiData);

  return (
    <>
      <h1>Your shortened URL </h1>
      <p>
        Copy the short link and share it in messages, texts, posts, websites and
        other locations.
      </p>
    </>
  );
}
