import { Component } from '@angular/core';
import { Admin } from '../admin/admin';
import { IOption } from '../interfaces/interfaces';
import { } from '@angular/material';

@Component({
  selector: 'ff-options',
  templateUrl: './options.component.html'
})
export class OptionsComponent {
  constructor(private admin: Admin) {
    this.fetchOptions();
  }
  public async fetchOptions(): Promise<void> {
    let opts = await this.admin.getOptions();
    this.data = opts;
  }
  public data: IOption[] = null;
  public displayedColumns : string[] = ["code","name","currency","fractions","badyear","update"]
}

@Component({
  selector: 'ff-option',
  templateUrl: './option.component.html'
})
export class OptionComponent {
  public data: IOption;
}