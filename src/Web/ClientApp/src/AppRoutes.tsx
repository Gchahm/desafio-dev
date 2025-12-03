import {ImportTransaction} from "./components/ImportTransaction";
import {ListTransactions} from "./components/ListTransactions";

interface RouteConfig {
    index?: boolean;
    path?: string;
    element: JSX.Element;
}

const AppRoutes: RouteConfig[] = [
    {
        index: true,
        element: <ListTransactions/>
    },
    {
        path: '/import',
        element: <ImportTransaction/>
    }
];

export default AppRoutes;
