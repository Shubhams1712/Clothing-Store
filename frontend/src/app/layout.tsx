import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import { QueryProvider } from "@/providers/query-provider";
import { TooltipProvider } from "@/components/ui/tooltip";
import { NavbarShell } from "@/components/layout/navbar-shell";
import { FooterShell } from "@/components/layout/footer-shell";
import { PageContainer } from "@/components/layout/page-container";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Store - Premium Clothing",
  description: "Premium clothing brand eCommerce platform",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="en"
      className={`${geistSans.variable} ${geistMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col">
        <QueryProvider>
          <TooltipProvider>
            <NavbarShell />
            <PageContainer>{children}</PageContainer>
            <FooterShell />
          </TooltipProvider>
        </QueryProvider>
      </body>
    </html>
  );
}
