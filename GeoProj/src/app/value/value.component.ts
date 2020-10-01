import { Component, OnInit } from '@angular/core';
import { Http } from '@angular/http';
import { Data } from 'global'

@Component({
  selector: 'app-value',
  templateUrl: './value.component.html',
  styleUrls: ['./value.component.less']
})
export class ValueComponent implements OnInit {
  result = null;
  popularResults = [];
  src: string = '';
  dest: string = '';
  isWaiting: boolean = false;
  hasError(): boolean {
    return this.result == -1;
  }

  enableClick(): boolean {
    return this.src != '' && this.dest != "";
  }
  show(): void {
    alert(this.dest)
  }
  constructor(private http: Http) { }

  ngOnInit(): void {
  }

  getDistance() {
    //reset some fields:
    this.isWaiting = true;
    this.result = null;
    this.popularResults = [];

    //load the distance:
    this.http.get(`${Data.uri}GetDistance/${this.src}/${this.dest}`).subscribe
      (response => {
        this.result = response.json();

        //load popular searches:
        this.http.get(`${Data.uri}PopularSearches/10`).subscribe
          (response => {
            this.popularResults = response.json();
            this.isWaiting = false;

          });
      }

      );


  }
}
