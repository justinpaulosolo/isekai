import { useState } from 'react';
// import aspireLogo from '/Aspire.png';
import './App.css';

// interface WeatherForecast {
//   date: string;
//   temperatureC: number;
//   temperatureF: number;
//   summary: string;
// }

function App() {
  const [longUrl, setLongUrl] = useState<string>('https://www.shorturl.at/');
  // useEffect(() => {}, []);

  const onShortenClick = async () => {
    // event.preventDefault();

    console.log(longUrl);
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
      console.log('Success:', result);
    } catch (error) {
      console.error(error);
    }

    //Need validation for
  };

  return (
    <div className="app-container">
      <h1 className="text-3xl font-bold underline">Short!</h1>

      {/* Need to change to url */}
      <input
        type="url"
        placeholder="Primary"
        className="input input-primary"
        name="longUrl"
        value={longUrl}
        onChange={(e) => setLongUrl(e.currentTarget.value)}
      />

      <button className="btn btn-outline btn-primary" onClick={onShortenClick}>
        Shorten
      </button>
      {/* 
      <a href="/shortennnn" target="_blank" rel="noopener noreferrer">
        HERE
      </a>
      <a className="link link-primary" href="/shortennnn">
        HERE
      </a> */}
    </div>
  );
}

export default App;
