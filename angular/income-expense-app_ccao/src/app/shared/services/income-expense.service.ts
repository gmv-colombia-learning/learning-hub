import { Injectable } from "@angular/core";

import {
  Firestore,
  doc,
  collection,
  addDoc,
  collectionData,
  deleteDoc,
  DocumentData,
  DocumentReference,
} from "@angular/fire/firestore";
import { IncomeExpense } from "../models/income-expense.model";
import { AuthService } from "./auth.service";

import { map } from "rxjs/operators";
import { Observable } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class IncomeExpenseService {
  constructor(
    private firestore: Firestore,
    private authService: AuthService,
  ) {}

  public createIncomeExpense(
    incomeExpense: IncomeExpense,
  ): Promise<DocumentReference<DocumentData>> {
    const uid = this.authService.user?.uid;

    delete incomeExpense.uid;

    const itemsCollection = collection(
      this.firestore,
      `${uid}/income-expense/items`,
    );
    return addDoc(itemsCollection, { ...incomeExpense });
  }

  public initIncomeExpenseListener(uid: string): Observable<any[]> {
    const itemsRef = collection(this.firestore, `${uid}/income-expense/items`);

    return collectionData(itemsRef, { idField: "uid" }).pipe(
      map((items) => items as any[]),
    );
  }

  public deleteIncomeExpense(uidItem: string): Promise<void> {
    const uid = this.authService.user?.uid;

    const docRef = doc(
      this.firestore,
      `${uid}/income-expense/items/${uidItem}`,
    );

    return deleteDoc(docRef);
  }
}
