import { Component } from '@angular/core';
import { Admin } from '../backend/admin';
import { ICharity } from '../interfaces/interfaces';

@Component({
  selector: 'ff-charities',
  templateUrl: './charities.component.html'
})
export class CharitiesComponent {
  constructor(private admin: Admin) {
    this.fetchCharities();
  }
  public async fetchCharities(): Promise<void> {
    let opts = await this.admin.getCharities();
    this.data = opts;
  }
  public data: ICharity[] = null;
  public displayedColumns : string[] = ["code","name","bank_name","bank_account_no","bank_bic"]
}
