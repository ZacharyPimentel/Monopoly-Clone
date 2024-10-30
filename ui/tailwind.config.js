/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors:{
        monopolyBrown: '#955436',
        monopolyLightBlue: '#b4e0f9',
        monopolyPink: '#d6348a',
        monopolyOrange: '#f59201',
        monopolyRed: '#df0c14',
        monopolyYellow:'#ffee02',
        monopolyGreen: '#01a84e',
        monopolyBlue: '#0167b2',

        totorodarkgreen: '#174632',
        totorogreen:'#4b8e52',
        totorolightgreen:'#7fd677',
        totoroyellow: '#eadb87',
        totorobeige: '#eeda9e',
        totorogrey: '#b8b8b8',
        totorored:'#FF7F7F'
      },
      fontFamily:{
        "totoro":"totoro",
      }
    },
  },
  plugins: [],
}

