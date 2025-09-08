/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{ts,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        brand: {
          50: '#eef6ff',
          100: '#d9eaff',
          200: '#bcd9ff',
          300: '#8fc2ff',
          400: '#5ea5ff',
          500: '#3185ff',
          600: '#2169db',
          700: '#1f56b1',
          800: '#1f478e',
          900: '#1f3c75',
        },
      },
    },
  },
  plugins: [],
}

