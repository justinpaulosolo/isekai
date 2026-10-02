import { Link } from '@tanstack/react-router';

import logo from '../../assets/screen.png';
import { useAuth } from '../../lib/auth.ts';
import { GoogleSignInBtn } from './google-signin.tsx';

export function Navbar() {
  const { user, isLoading, login, logout } = useAuth();

  return (
    <div className="navbar relative z-50 bg-base-100 backdrop-blur-md backdrop-filter border-b border-zinc-800/80 px-10">
      <div className="navbar-start">
        <div className="dropdown">
          <div tabIndex={0} role="button" className="btn btn-ghost lg:hidden">
            <svg
              aria-label="Menu"
              xmlns="http://www.w3.org/2000/svg"
              className="h-5 w-5"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              {' '}
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M4 6h16M4 12h8m-8 6h16"
              />{' '}
            </svg>
          </div>
          <ul
            tabIndex={-1}
            className="menu menu-sm dropdown-content bg-base-100 rounded-box z-50 mt-3 w-52 p-2 shadow"
          >
            <li>
              <a>Features</a>
            </li>

            <li>
              <a>Analytics</a>
            </li>

            <li>
              <a>Pricing</a>
            </li>

            <li>
              <a>API</a>
            </li>

            <li>
              <a>GitHub</a>
            </li>

            <li>
              <GoogleSignInBtn login={login} />
            </li>
          </ul>
        </div>

        <Link
          to="/"
          className="flex text-xl items-center text-white whitespace-nowrap me-3"
        >
          <img src={logo} alt="" className="w-8 me-1" />
          SnapLink
        </Link>

        <div className="hidden lg:inline-flex items-center gap-2 px-3 py-1 text-xs font-medium bg-base-300 text-sky-400 border border-slate-50/20 rounded-full backdrop-blur-sm font-mono">
          v0.0.1
        </div>
      </div>

      {/* Nav Center */}
      <div className="navbar-center hidden lg:flex">
        <ul className="menu menu-horizontal px-1 ">
          <li>
            <Link to="/" className="nav-link">
              Features
            </Link>
          </li>

          <li>
            <Link to="/" className="nav-link">
              Analytics
            </Link>
          </li>

          <li>
            <Link to="/" className="nav-link">
              Pricing
            </Link>
          </li>

          <li>
            <Link to="/" className="nav-link">
              API
            </Link>
          </li>

          <li>
            <Link to="/" className="nav-link">
              GitHub
            </Link>
          </li>
        </ul>
      </div>

      {/* Nav End */}
      <div className="navbar-end gap-5">
        {isLoading ? (
          ''
        ) : user ? (
          <div className="flex gap-2">
            <div className="dropdown dropdown-end">
              <div
                tabIndex={0}
                role="button"
                className="btn btn-ghost btn-circle avatar"
              >
                <div className="w-10 rounded-full">
                  <img
                    alt="Tailwind CSS Navbar component"
                    src="https://img.daisyui.com/images/stock/photo-1534528741775-53994a69daeb.webp"
                  />
                </div>
              </div>
              <ul
                tabIndex={-1}
                className="menu menu-sm dropdown-content bg-base-200 rounded-box z-1 mt-3 w-52 p-2 shadow"
              >
                <li>
                  <Link to="/dashboard" className="justify-between">
                    Dashboard
                    <span className="badge">New</span>
                  </Link>
                </li>
                <li>
                  <a onClick={logout}>Logout</a>
                </li>
              </ul>
            </div>
          </div>
        ) : (
          <GoogleSignInBtn login={login} className="hidden lg:flex" />
        )}
      </div>
    </div>
  );
}
