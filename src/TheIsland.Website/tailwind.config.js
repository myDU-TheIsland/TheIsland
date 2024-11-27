module.exports = {
  content: ['./**/*.cshtml', './Pages/**/*.cshtml', './Views/**/*.cshtml'],
  theme: {
    extend: {
      colors: {
        darkBlue: '#23283e',
        midnightBlue: '#1a1a2e',
      },
      spacing: {
        30: '7rem',
        35: '7.5rem',
        100: '28rem',
      },
      transformOrigin: {
        'top-left': 'top left',
      },
      screens: {
        xs: '340px',
      },
    },
  },
  plugins: [],
};
