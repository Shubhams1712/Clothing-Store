import Link from "next/link";

export default function NotFound() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center text-center">
      <h1 className="mb-2 text-7xl font-bold tracking-tight">404</h1>
      <p className="mb-2 text-sm font-semibold uppercase tracking-wider text-neutral-400">Page Not Found</p>
      <p className="mb-8 max-w-md text-sm text-neutral-500">
        The page you are looking for does not exist or has been moved.
      </p>
      <Link
        href="/"
        className="inline-flex h-12 items-center bg-black px-6 text-sm font-bold uppercase tracking-wider text-white transition-colors hover:bg-neutral-800"
      >
        Back to Home
      </Link>
    </div>
  );
}
