import { defineConfig } from 'orval';

export default defineConfig({
  fitsync: {
    input: {
      target: './swagger.json',
    },
    output: {
      mode: 'tags-split',
      target: './src/api/generated',
      client: 'react-query',
      override: {
        mutator: {
          path: './src/api/axios-instance.ts',
          name: 'customAxiosInstance',
        },
      },
    },
  },
});
