import { Link } from '@tanstack/react-router';

import logo from '../../assets/screen.png';

export function Footer() {
  return (
    <footer className="footer sm:footer-horizontal bg-base-200 text-base-content p-10">
      <aside>
        <Link to="/" className="text-xl flex me-2 text-white">
          <img src={logo} alt="" className="w-8 me-1" />
          SnapLink
        </Link>
      </aside>
      <nav>
        <h6 className="font-bold uppercase text-white">Products</h6>
        <button type="button" className="nav-link">
          Link Engine
        </button>
        <button type="button" className="nav-link">
          Live Telemetry
        </button>
        <button type="button" className="nav-link">
          Enterprise
        </button>
        <button type="button" className="nav-link">
          QR Engine
        </button>
      </nav>
      <nav>
        <h6 className="font-bold uppercase">Developers</h6>
        <button type="button" className="nav-link">
          REST API
        </button>
        <button type="button" className="nav-link">
          SDK
        </button>
        <button type="button" className="nav-link">
          GitHub Repo
        </button>
        <button type="button" className="nav-link">
          Docs
        </button>
      </nav>
      <nav>
        <h6 className="font-bold uppercase text-white">Company</h6>
        <button type="button" className="nav-link">
          About Us
        </button>
        <button type="button" className="nav-link">
          Contact
        </button>
        <button type="button" className="nav-link">
          Jobs
        </button>
      </nav>
    </footer>
  );
}
