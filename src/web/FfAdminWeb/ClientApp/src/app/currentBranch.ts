export class CurrentBranch {
  constructor(){
    let x = this.getBranchName();
    if(!x)
    {
      this.setBranchName("Main");
    }
  }

  public getBranchName(): string {
    return localStorage.getItem("branchName");
  }

  public setBranchName(branchName: string): void {
    localStorage.setItem("branchName", branchName);
  }
}
