/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      spacing:{
        1:'4px',
        2:'8px',
        3:'12px',
        4:'16px',
        5:'24px',
        6:'32px',
        7:'48px',
        8:'64px'
      },
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
        totorored:'#FF7F7F',

        primary1: "var(--primary1)",
        primary2: "var(--primary2)",
        primary3: "var(--primary3)",
        primary4: "var(--primary4)",
        primary5: "var(--primary5)",
        primary6: "var(--primary6)",
        primary7: "var(--primary7)",
        primary8: "var(--primary8)",
        primary9: "var(--primary9)",
        primary10: "var(--primary10)",
      },
      fontFamily:{
        "totoro":"totoro",
      },
    },
  },
  plugins: [],
}

