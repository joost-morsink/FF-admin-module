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
  selector: 'ff-import-events',
  templateUrl: './importEvents.component.html'
})
export class ImportEventsComponent {
  constructor(private eventStore: EventStore, private dialog: MatDialog) { }
  public file: File;
  public fileName: string;
  private fileInput: HTMLInputElement;

  public onFileSelected(e: Event) {
    this.fileInput = e.target as HTMLInputElement;
    this.file = this.fileInput.files[0];
    this.fileName = this.file?.name;
  }

  public async executeUpload() {
    if (this.file) {
      const formData = new FormData();
      formData.append("file", this.file);

      try {
        let content = await this.file.text();

        await this.eventStore.importEvents(JSON.parse(content) as IFullEvent[]);

        this.dialog.open(InfoDialog, {
          data: { title: "Success", message: "Import and processing successful!" }
        });
      } catch (ex) {
        this.dialog.open(ErrorDialog, {
          data: { errors: ex.error }
        })
      }
    }
  }
}
