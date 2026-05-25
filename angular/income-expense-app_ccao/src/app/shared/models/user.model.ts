export interface User {
  uid: string;
  name: string;
  email: string | null;
}

export const userFromFirebase = ({ uid, name, email }: User): User => ({
  uid,
  name,
  email,
});
