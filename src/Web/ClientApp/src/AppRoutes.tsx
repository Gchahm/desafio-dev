import {Home} from "./components/Home";
import {ImportTransaction} from "./components/ImportTransaction";

interface RouteConfig {
    index?: boolean;
    path?: string;
    element: JSX.Element;
}

const AppRoutes: RouteConfig[] = [
    {
        index: true,
        element: <Home/>
    },
    {
        path: '/import',
        element: <ImportTransaction/>
    }
];

export default AppRoutes;
