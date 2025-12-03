import React from 'react';
import {CnabImportResult, StoreTransactionsDto, TransactionsClient} from '../web-api-client';
import {StoreCard} from "./StoreCard";
import {StoreList} from "./StoreList";

interface ImportTransactionState {
    selectedFile: File | null;
    uploading: boolean;
    result: CnabImportResult | null;
    error: string | null;
}

const client = new TransactionsClient();

export const ListTransactions = () => {

    const [stores, setStores] = React.useState<StoreTransactionsDto[]>([]);
    const [selectedStore, setSelectedStore] = React.useState<StoreTransactionsDto | null>(null);

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
