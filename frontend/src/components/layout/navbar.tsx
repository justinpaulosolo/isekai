import {Link} from '@tanstack/react-router';

import logo from '../../assets/screen.png';
import {useAuth} from "../../lib/auth.ts";

export function Navbar() {
  const { user, isLoading, login, logout } = useAuth();
  return (
    <div className="navbar bg-base-100 backdrop-blur-md backdrop-filter border-b border-zinc-800/80 px-10">
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
            className="menu menu-sm dropdown-content bg-base-100 rounded-box z-1 mt-3 w-52 p-2 shadow"
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
          </ul>
        </div>

        <Link to="/" className="text-xl flex me-2 text-white">
          <img src={logo} alt="" className="w-8 me-1" />
          SnapLink
        </Link>
        <div className="inline-flex items-center gap-2 px-3 py-1 text-xs font-medium bg-base-300 text-sky-400 border border-slate-50/20 rounded-full backdrop-blur-sm font-mono">
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
        {
          isLoading ? (
              ""
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
              <button onClick={() => login()} className="btn bg-white text-black border-[#e5e5e5]">
                <svg aria-label="Google logo" width="16" height="16" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512"><g><path d="m0 0H512V512H0" fill="#fff"></path><path fill="#34a853" d="M153 292c30 82 118 95 171 60h62v48A192 192 0 0190 341"></path><path fill="#4285f4" d="m386 400a140 175 0 0053-179H260v74h102q-7 37-38 57"></path><path fill="#fbbc02" d="m90 341a208 200 0 010-171l63 49q-12 37 0 73"></path><path fill="#ea4335" d="m153 219c22-69 116-109 179-50l55-54c-78-75-230-72-297 55"></path></g></svg>
                Login with Google
              </button>
          )
        }
      </div>
    </div>
  );
}
