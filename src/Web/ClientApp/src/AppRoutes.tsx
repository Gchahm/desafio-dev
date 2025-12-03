import { FetchData } from "./components/FetchData";
import { Home } from "./components/Home";

interface RouteConfig {
    index?: boolean;
    path?: string;
    element: JSX.Element;
}

const AppRoutes: RouteConfig[] = [
    {
        index: true,
        element: <Home />
    },
    {
        path: '/fetch-data',
        element: <FetchData />
    }
];

export default AppRoutes;
