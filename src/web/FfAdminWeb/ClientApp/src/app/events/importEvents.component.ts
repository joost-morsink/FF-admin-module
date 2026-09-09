import { Component, ChangeDetectionStrategy } from '@angular/core';
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
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './importEvents.component.html'
})
export class ImportEventsComponent {
  constructor(private eventStore: EventStore, private dialog: MatDialog) { }
  public file: File | null = null;
  public fileName: string | null = null;
  private fileInput: HTMLInputElement | null = null;

  public onFileSelected(e: Event) {
    this.fileInput = e.target as HTMLInputElement;
    this.file = this.fileInput.files?.[0] ?? null;
    this.fileName = this.file?.name ?? null;
  }

  public async executeUpload() {
    if (this.file) {
      const formData = new FormData();
      formData.append("file", this.file);

      try {
        let content = await this.file.text();
        let events= content.split('\n').map(l => JSON.parse(l) as IFullEvent);
        await this.eventStore.importEvents(events);

        this.dialog.open(InfoDialog, {
          data: { title: "Success", message: "Import and processing successful!" }
        });
      } catch (ex: any) {
        this.dialog.open(ErrorDialog, {
          data: { errors: ex.error }
        })
      }
    }
  }
}
