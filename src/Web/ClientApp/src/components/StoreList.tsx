import {ListGroup, ListGroupItem} from "reactstrap";
import React from "react";
import {StoreDto} from "../web-api-client";
import {Money} from "./Money";

interface StoreListProps {
    stores: StoreDto[];
    selectedStore: StoreDto | null;
    setSelectedStore: (store: StoreDto) => void;
}

export const StoreList = (props: StoreListProps) => {
    const {stores, selectedStore, setSelectedStore} = props;

    return (
        <ListGroup>
            {stores.map((s) => (
                <ListGroupItem
                    key={s.id}
                    action
                    active={selectedStore?.id === s.id}
                    onClick={() => setSelectedStore(s)}
                    className="d-flex justify-content-between align-items-center"
                >
                    <div className="me-2">
                        <div className="fw-semibold">{s.name}</div>
                        <small>{s.ownerName}</small>
                    </div>
                    <span className={`badge rounded-pill ${(s.totalBalance ?? 0) < 0 ? 'bg-danger' : 'bg-success'}`}>
                            <Money value={s.totalBalance}/>
                    </span>
                </ListGroupItem>
            ))}
            {stores.length === 0 && (
                <ListGroupItem disabled>No stores found</ListGroupItem>
            )}
        </ListGroup>
    )
}