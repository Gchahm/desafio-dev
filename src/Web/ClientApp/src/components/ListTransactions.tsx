import React from 'react';
import {CnabImportResult, StoreDto, TransactionsClient} from '../web-api-client';
import {StoreCard} from "./StoreCard";
import {StoreList} from "./StoreList";

const client = new TransactionsClient();

export const ListTransactions = () => {

    const [stores, setStores] = React.useState<StoreDto[]>([]);
    const [selectedStore, setSelectedStore] = React.useState<StoreDto | null>(null);

    React.useEffect(() => {
        client.getStoreTransactions().then((res) => {
            setStores(res);
            if (res && res.length > 0) {
                setSelectedStore(res[0]);
            }
        });
    }, []);


    return (
        <div className="container-fluid">
            <h1 className="mb-4">Transactions</h1>
            <div className="row">
                <div className="col-12 col-md-4 col-lg-3 mb-3">
                    <StoreList stores={stores} selectedStore={selectedStore} setSelectedStore={setSelectedStore}/>
                </div>
                <div className="col-12 col-md-8 col-lg-9">
                    {selectedStore ? (
                        <StoreCard store={selectedStore}/>
                    ) : (
                        <div className="text-muted">Select a store to view its transactions.</div>
                    )}
                </div>
            </div>
        </div>
    );
}
