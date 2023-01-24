import { Component } from '@angular/core';
import { Admin } from '../backend/admin';
import {ICharity, IEventCharityPartition} from '../interfaces/interfaces';
import {ActivatedRoute, Router} from "@angular/router";
import {EventStore} from "../backend/eventstore";
import {FormControl, ValidationErrors} from "@angular/forms";
import {ErrorDialog} from "../dialogs/error.dialog";
import {MatDialog} from "@angular/material/dialog";

@Component({
  selector: 'ff-charity-partition',
  templateUrl: './charity_partition.component.html'
})
export class CharityPartitionComponent {
  constructor(private eventStore: EventStore, private route:ActivatedRoute, private router: Router, private dialog: MatDialog, private admin: Admin) {
    this.fetchCharities();
  }
  public async fetchCharities(): Promise<void> {
    let opts = await this.admin.getCharities();
    let current = await this.admin.getCharityPartitions(this.route.snapshot.params.id);
    this.data = opts.map(o => { return { checked: current.find(fs => fs.holder == o.code) != null, item: o }; });
    this.subject = opts.find(o => o.id == this.route.snapshot.params.id);
  }
  public subject: ICharity = null;
  public data: {checked:boolean, item:ICharity}[] = null;
  public displayedColumns : string[] = ["checked","code","name","bank_name","bank_account_no","bank_bic"]
  public async save() {
    let ids = this.data.filter(o => { console.log(o); return o.checked; }).map(o => o.item.code);
    console.log(ids);
    let event : IEventCharityPartition = { type: 'META_CHARITY_PARTITION',
      timestamp: new Date(),
      charity: this.subject.code,
      partitions: ids.map(c => { return { holder: c, fraction: 1 } })};
    try {
      await this.eventStore.postEvent(event);
      await this.router.navigate(["charities"]);
    } catch (ex) {
      this.dialog.open(ErrorDialog, {
        data: { errors: ex.error },
      });
    }
  }
}
