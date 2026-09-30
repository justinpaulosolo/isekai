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
        <a className="nav-link">Link Engine</a>
        <a className="nav-link">Live Telemetry</a>
        <a className="nav-link">Enterprise</a>
        <a className="nav-link">QR Engine</a>
      </nav>
      <nav>
        <h6 className="font-bold uppercase">Developers</h6>
        <a className="nav-link">REST API</a>
        <a className="nav-link">SDK</a>
        <a className="nav-link">GitHub Repo</a>
        <a className="nav-link">Docs</a>
      </nav>
      <nav>
        <h6 className="font-bold uppercase text-white">Company</h6>
        <a className="nav-link">About Us</a>
        <a className="nav-link">Contact</a>
        <a className="nav-link">Jobs</a>
      </nav>
    </footer>
  );
}
