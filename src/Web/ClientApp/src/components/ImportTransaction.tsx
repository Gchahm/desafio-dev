import React, {ChangeEvent, FormEvent} from 'react';
import {CnabImportResult, TransactionsClient} from '../web-api-client';
import {Alert, Button, Form, FormGroup, Input, Label} from 'reactstrap';
import {useMutation} from "@tanstack/react-query";
import { ImportResult } from "./ImportResult";


const client = new TransactionsClient();

export const ImportTransaction = () => {
    const [selectedFile, setSelectedFile] = React.useState<File | null>(null);
    const [ignoreErrors, setIgnoreErrors] = React.useState(false);
    const [result, setResult] = React.useState<CnabImportResult | null>(null);
    const [error, setError] = React.useState<string | null>(null);

    const mutation = useMutation({
        mutationFn: async () => {
            if (!selectedFile) {
                setError('Please select a file to upload.');
                return;
            }

            return await client.importCnabFile(ignoreErrors, {
                data: selectedFile,
                fileName: selectedFile.name
            });
        },
        onSuccess: (data) => {
            const fileInput = document.getElementById('fileInput') as HTMLInputElement;
            if (fileInput) {
                fileInput.value = '';
            }
            data && setResult(data);
        },
        onError: (error) => {
            setError(error.message || 'An error occurred while uploading the file.');
        }
    });

    const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
        const files = event.target.files;
        if (files && files.length > 0) {
            setSelectedFile(files[0]);
        }
    };

    const handleIgnoreErrorsChange = (event: ChangeEvent<HTMLInputElement>) => {
        setIgnoreErrors(event.target.checked);
    };

    const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
        event.preventDefault();
        await mutation.mutateAsync();
    }

    return (
        <div>
            <h1>Importar arquivo CNAB</h1>

            <Form onSubmit={handleSubmit}>
                <FormGroup>
                    <Label for="fileInput">Selecione o arquivo CNAB</Label>
                    <Input
                        id="fileInput"
                        type="file"
                        onChange={handleFileChange}
                        disabled={mutation.isPending}
                        accept=".txt,.cnab,.rem,.ret"
                    />
                    {selectedFile && (
                        <small className="form-text text-muted">
                            Selected: {selectedFile.name} ({(selectedFile.size / 1024).toFixed(2)} KB)
                        </small>
                    )}
                </FormGroup>

                <FormGroup check>
                    <Label check>
                        <Input
                            type="checkbox"
                            checked={ignoreErrors}
                            onChange={handleIgnoreErrorsChange}
                            disabled={mutation.isPending}
                        />{' '}
                       Ignorar linhas com errors e salvar arquivo no banco de dados.
                    </Label>
                </FormGroup>

                <Button
                    color="primary"
                    type="submit"
                    disabled={!selectedFile || mutation.isPending}
                >
                    {mutation.isPending ? 'Enviando...' : 'Enviar'}
                </Button>
            </Form>

            {error && (
                <Alert color="danger" className="mt-3">
                    {error}
                </Alert>
            )}

            {result && (
                <ImportResult result={result}/>
            )}
        </div>
    );
}



