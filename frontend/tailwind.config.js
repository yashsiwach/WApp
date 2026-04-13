/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './src/**/*.{html,ts}',
  ],
  theme: {
    extend: {
      fontFamily: {
        body: ['Outfit', 'Segoe UI', 'sans-serif'],
        display: ['Epilogue', 'Segoe UI', 'sans-serif'],
      },
      colors: {
        navy: {
          DEFAULT: '#1E3A8A',
          dark: '#172554',
          light: '#1D4ED8',
        },
        accent: {
          DEFAULT: '#2563EB',
          hover: '#1D4ED8',
          soft: '#93C5FD',
        },
        surface: {
          950: '#F8FAFF',
          900: '#EEF4FF',
          800: '#E3EDFF',
          700: '#CBDDFF',
          600: '#AFC9FF',
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
