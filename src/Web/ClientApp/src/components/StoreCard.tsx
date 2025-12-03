import {StoreTransactionsDto} from "../web-api-client";
import {Card, Table} from "reactstrap";
import {Money} from "./Money";


export interface StoreProps {
    store: StoreTransactionsDto;
}

export const StoreCard = (props: StoreProps) => {
    const {store} = props;

    return (
        <Card>
            <Table>
                <thead className="bg-light">
                <tr>
                    <th>Data</th>
                    <th>Horario</th>
                    <th>Tipo</th>
                    <th>CPF</th>
                    <th>Cartao</th>
                    <th className="text-end">Valor</th>
                </tr>
                </thead>
                <tbody>
                {store.transactions?.map((transaction) => (
                    <tr>
                        <td>{transaction.date?.toLocaleDateString()} </td>
                        <td>{transaction.date?.toLocaleTimeString()}</td>
                        <td>{transaction.description}</td>
                        <td>{transaction.cpf}</td>
                        <td>{transaction.cardNumber}</td>
                        <td className={`text-end ${transaction.nature == 'Expense' ? 'text-danger' : 'text-success'}`}>
                            <Money value={transaction.signedAmount}/>
                        </td>
                    </tr>
                ))}
                </tbody>
                <tfoot className="bg-light">
                <tr>
                    <td colSpan={5}>Total</td>
                    <td className="text-end">
                        <Money value={store.totalBalance}/>
                    </td>
                </tr>
                </tfoot>
            </Table>
        </Card>
    )
}