import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'firstTwoLetters',
  standalone: true,
})
export class FirstTwoLettersPipe implements PipeTransform {
  transform(user: string): string {
    let names = user?.split(' ')
    let initials = names?.map((name) => name.charAt(0).toUpperCase()).join('')
    return initials?.substring(0, 2)
  }
}
