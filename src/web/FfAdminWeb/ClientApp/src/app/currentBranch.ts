export class CurrentBranch {
  constructor(){
    let x = this.getBranchName();
    if(!x)
    {
      this.setBranchName("Please select branch");
    }
  }

  public getBranchName(): string {
    return localStorage.getItem("branchName") ?? "Please select branch";
  }

  public setBranchName(branchName: string): void {
    localStorage.setItem("branchName", branchName);
  }
}
