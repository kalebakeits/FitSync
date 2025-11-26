import axios, { type AxiosRequestConfig } from "axios";

export const AXIOS_INSTANCE = axios.create({
  baseURL: import.meta.env.VITE_API_URL || "",
  withCredentials: true,
});

// Handle 401 responses - redirect to login only if not already on auth pages
AXIOS_INSTANCE.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const currentPath = window.location.pathname;
      const isAuthPage =
        currentPath === "/login" || currentPath === "/register";

      if (!isAuthPage) {
        console.log(
          "[Axios Interceptor] 401 received on protected page - redirecting to login",
        );
        window.location.href = "/login";
      } else {
        console.log(
          "[Axios Interceptor] 401 received on auth page - letting error bubble up",
        );
      }
    }
    return Promise.reject(error);
  },
);

export const customAxiosInstance = <T>(
  config: AxiosRequestConfig,
): Promise<T> => {
  const source = axios.CancelToken.source();
  const promise = AXIOS_INSTANCE({
    ...config,
    cancelToken: source.token,
  }).then(({ data }) => data);

  // @ts-ignore
  promise.cancel = () => {
    source.cancel("Query was cancelled");
  };

  return promise;
};

export default customAxiosInstance;
