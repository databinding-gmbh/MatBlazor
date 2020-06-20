import {MDCTextField} from '@material/textfield';

export class MatTextField {
  constructor(ref) {
      const textField = new MDCTextField(ref);
      textField.foundation_.styleValidity_ = function() {
          // Nothing to do
      };
    }  
}

export function init(ref) {
  new MatTextField(ref);
}

