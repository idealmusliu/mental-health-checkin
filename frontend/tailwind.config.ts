import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/app/**/*.{ts,tsx}",
    "./src/components/**/*.{ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // Mood palette: red (low) -> green (great).
        mood: {
          1: "#ef4444",
          2: "#f97316",
          3: "#eab308",
          4: "#84cc16",
          5: "#22c55e",
        },
      },
    },
  },
  plugins: [],
};

export default config;
