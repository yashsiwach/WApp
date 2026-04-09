/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/**/*.{html,ts}',
  ],
  theme: {
    extend: {
      fontFamily: {
        body: ['Manrope', 'Segoe UI', 'sans-serif'],
        display: ['Sora', 'Segoe UI', 'sans-serif'],
      },
      colors: {
        navy: {
          DEFAULT: '#18181B',
          dark: '#09090B',
          light: '#27272A',
        },
        accent: {
          DEFAULT: '#F59E0B',
          hover: '#D97706',
          soft: '#FCD34D',
        },
        surface: {
          950: '#09090B',
          900: '#111215',
          800: '#1A1D22',
          700: '#262A31',
          600: '#3A404A',
        },
        tier: {
          bronze: '#CD7F32',
          silver: '#C0C0C0',
          gold: '#FFD700',
          platinum: '#E5E4E2',
        },
      },
    },
  },
  plugins: [],
};
