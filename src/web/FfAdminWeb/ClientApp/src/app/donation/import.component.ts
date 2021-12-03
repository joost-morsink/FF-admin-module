import { Component } from '@angular/core';
import { Admin } from '../backend/admin';
import { EventStore } from '../backend/eventstore';
import { IOption, IEventNewOption, IValidationMessage, IEventStatistics, IFullEvent } from '../interfaces/interfaces';
import { FormControl, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { ErrorDialog } from '../dialogs/error.dialog';
import { InfoDialog } from '../dialogs/info.dialog';

@Component({
  selector: 'ff-import-csv',
  templateUrl: './import.component.html'
})
export class ImportCsvComponent {
  constructor(private eventStore: EventStore, private dialog: MatDialog) { }
  public file: File;
  public mollie: File;
  public fileName: string;
  public mollieFileName: string;
  private fileInput: HTMLInputElement;
  private mollieInput: HTMLInputElement;

  public onFileSelected(e) {
    this.fileInput = e.target;
    this.file = this.fileInput.files[0];
    this.fileName = this.file?.name;
  }

  public onMollieFileSelected(e) {
    this.mollieInput = e.target;
    this.mollie = this.mollieInput.files[0];
    this.mollieFileName = this.mollie?.name;
  }

  public async executeUpload() {
    if (this.file) {
      const formData = new FormData();
      formData.append("file", this.file);
      formData.append("mollie", this.mollie);

      try {
        await this.eventStore.importCsv(formData);
        await this.eventStore.process();
        this.fileInput.files = null;
        this.file = null;
        this.fileName = null;

        this.dialog.open(InfoDialog, {
          data: { title: "Success", message: "Import and processing successful!" }
        });
      } catch (ex) {
        this.dialog.open(ErrorDialog, {
          data: { errors: ex.error }
        }).afterClosed().toPromise();
      }
    }
  }
}