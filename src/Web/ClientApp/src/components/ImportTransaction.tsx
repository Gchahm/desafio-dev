import React, { Component, ChangeEvent, FormEvent } from 'react';
import { TransactionsClient, CnabImportResult, FileParameter } from '../web-api-client';
import { Button, Form, FormGroup, Label, Input, Alert, Table } from 'reactstrap';

interface ImportTransactionState {
  selectedFile: File | null;
  uploading: boolean;
  result: CnabImportResult | null;
  error: string | null;
}

export class ImportTransaction extends Component<{}, ImportTransactionState> {
  static displayName = ImportTransaction.name;

  constructor(props: {}) {
    super(props);
    this.state = {
      selectedFile: null,
      uploading: false,
      result: null,
      error: null
    };
  }

  handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.setState({
        selectedFile: files[0],
        result: null,
        error: null
      });
    }
  };

  handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    const { selectedFile } = this.state;

    if (!selectedFile) {
      this.setState({ error: 'Please select a file to upload.' });
      return;
    }

    this.setState({ uploading: true, error: null, result: null });

    try {
      const client = new TransactionsClient();
      const fileParameter: FileParameter = {
        data: selectedFile,
        fileName: selectedFile.name
      };

      const result = await client.importCnabFile(fileParameter);
      this.setState({
        result,
        uploading: false,
        selectedFile: null
      });

      // Reset the file input
      const fileInput = document.getElementById('fileInput') as HTMLInputElement;
      if (fileInput) {
        fileInput.value = '';
      }
    } catch (error: any) {
      this.setState({
        error: error.message || 'An error occurred while uploading the file.',
        uploading: false
      });
    }
  };

  renderResult() {
    const { result } = this.state;

    if (!result) return null;

    const alertType = result.isSuccess ? 'success' : 'warning';

    return (
      <div className="mt-4">
        <Alert color={alertType}>
          <h4 className="alert-heading">
            {result.isSuccess ? 'Import Successful!' : 'Import Completed with Issues'}
          </h4>
        </Alert>

        <Table striped bordered>
          <tbody>
            <tr>
              <th>Total Lines</th>
              <td>{result.totalLines ?? 0}</td>
            </tr>
            <tr>
              <th>Successful Imports</th>
              <td>{result.successfulImports ?? 0}</td>
            </tr>
            <tr>
              <th>Failed Imports</th>
              <td>{result.failedImports ?? 0}</td>
            </tr>
            <tr>
              <th>Stores Processed</th>
              <td>{result.storesProcessed ?? 0}</td>
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

  render() {
    const { selectedFile, uploading, error } = this.state;

    return (
      <div>
        <h1>Import CNAB File</h1>
        <p>Upload a CNAB file to import financial transactions.</p>

        <Form onSubmit={this.handleSubmit}>
          <FormGroup>
            <Label for="fileInput">Select CNAB File</Label>
            <Input
              id="fileInput"
              type="file"
              onChange={this.handleFileChange}
              disabled={uploading}
              accept=".txt,.cnab,.rem,.ret"
            />
            {selectedFile && (
              <small className="form-text text-muted">
                Selected: {selectedFile.name} ({(selectedFile.size / 1024).toFixed(2)} KB)
              </small>
            )}
          </FormGroup>

          <Button
            color="primary"
            type="submit"
            disabled={!selectedFile || uploading}
          >
            {uploading ? 'Uploading...' : 'Upload and Import'}
          </Button>
        </Form>

        {error && (
          <Alert color="danger" className="mt-3">
            {error}
          </Alert>
        )}

        {this.renderResult()}
      </div>
    );
  }
}
