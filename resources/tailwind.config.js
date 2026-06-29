// tailwind.config.js
module.exports = {
  theme: {
    extend: {
      colors: {
        dodRed: {
          '50': '#FFEDED',
          '100': '#FFC4C6',
          '200': '#FF9C9E',
          '300': '#FF7476',
          '400': '#FF4D4E',
          '500': '#E31C23', // Primary brand red
          '600': '#D4141A',
          '700': '#B30F13',
          '800': '#910C10',
          '900': '#71090C',
        },
        dragonGreen: {
          '500': '#18B868', // Base mist green
          // ...scale
        },
        runeCyan: {
          '500': '#34F2CF', // Central glow cyan
          // ...scale
        },
        charcoal: {
          '500': '#121212', // Near-black background/frame
          // ...scale
        },
        boneWhite: {
          '500': '#E2E8F0', // Pale accent/text color
          // ...scale
        },
      },
    },
  },
};