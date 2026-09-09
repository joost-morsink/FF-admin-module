import {Component, ChangeDetectionStrategy, ChangeDetectorRef} from '@angular/core';
import { Admin } from '../backend/admin';
import { ICharity } from '../interfaces/interfaces';

@Component({
  selector: 'ff-charities',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './charities.component.html'
})
export class CharitiesComponent  {
  constructor(private admin: Admin, private cdr: ChangeDetectorRef) {
    this.view = "main";
    this.fetchCharities();
  }
  public async fetchCharities(): Promise<void> {
    let opts = await this.admin.getCharities();
    this.data = opts;
    this.cdr.detectChanges();
  }
  public view: string;
  public data: ICharity[] | null = null;
  public displayedColumns : string[] = ["code","name","bank_name","bank_account_no","bank_bic","link"]
}
