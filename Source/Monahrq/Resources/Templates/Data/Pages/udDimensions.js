$.monahrq.udDimensions = {
  'geo': [
    {id: 'county', name: 'County', reportTypes: ['ed', 'id', 'county']},
    {id: 'region', name: 'Geographic Region', reportTypes: ['id', 'ed']},
    {id: 'zip', name: 'Zip Code', reportTypes: []},
    {id: 'hospital', name: 'Hospital Name', reportTypes: ['id', 'ed']},
    {id: 'hospitalType', name: 'Type of Hospital', reportTypes: ['id', 'ed']}
  ],
  'clinical': [
    {id: 'condition', name: 'Health Condition or Topic', reportTypes: ['id', 'ed', 'county']},
    {id: 'mdc', name: 'Major Diagnosis Category', reportTypes: ['id', 'county']},
    {id: 'drg', name: 'Diagnostic Related Group', reportTypes: ['id', 'county']},
    {id: 'procedure', name: 'Procedure', reportTypes: ['id', 'county']}
  ]
}

