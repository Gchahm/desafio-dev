import {CnabImportResult} from "../web-api-client";
import {Alert, Table} from "reactstrap";
import React from "react";

interface ResultComponentProps {
    result: CnabImportResult;
}

export const ImportResult = (props: ResultComponentProps) => {
    const {result} = props;

    const alertType = result.isSuccess ? 'success' : 'warning';

    return (
        <div className="mt-4">
            <Alert color={alertType}>
                <h4 className="alert-heading">
                    {result.isSuccess ? 'Arquivo salvo no banco de dados!' : 'Arquivo com erros!'}
                </h4>
            </Alert>

            <Table striped bordered>
                <tbody>
                <tr>
                    <th>Total Lines</th>
                    <td>{result.totalLines ?? 0}</td>
                </tr>
                <tr>
                    <th>Linhas validas</th>
                    <td>{result.validLines ?? 0}</td>
                </tr>
                <tr>
                    <th>Linhas com Error</th>
                    <td>{result.invalidLines ?? 0}</td>
                </tr>
                </tbody>
            </Table>

            {result.errors && result.errors.length > 0 && (
                <div>
                    <h5>Errors:</h5>
                    <Alert color="danger">
                        <ul className="mb-0">
                            {result.errors.map((error, index) => (
                                <li key={index}>{error}</li>
                            ))}
                        </ul>
                    </Alert>
                </div>
            )}
        </div>
    );
}
