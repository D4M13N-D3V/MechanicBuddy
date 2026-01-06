import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  reactStrictMode: true,
  poweredByHeader: false,
  output: "standalone", // For Docker deployment
};

export default nextConfig;
