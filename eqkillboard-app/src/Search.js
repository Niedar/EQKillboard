import React, { Component } from 'react';
import Autosuggest from 'react-autosuggest';
import { withApollo } from 'react-apollo';
import gql from "graphql-tag"


// Imagine you have a list of languages that you'd like to autosuggest.
const languages = [
  {
    name: 'C',
    year: 1972
  },
  {
    name: 'Elm',
    year: 2012
  },
];

// Teach Autosuggest how to calculate suggestions for any given input value.
const getSuggestions = (apolloClient, value) => {
    return apolloClient.query({
        query: GET_SEARCHALL,
        variables: { searchText: value}
    }).then(result => {
        let results = [];
        if (result.data) {
            results = results.concat(result.data.searchCharacters.nodes.map(node => { return {name: node.name}}));
            results =results.concat(result.data.searchGuilds.nodes.map(node => {return {name: node.name}}));
        }
        return results;
    })
};

// When suggestion is clicked, Autosuggest needs to populate the input
// based on the clicked suggestion. Teach Autosuggest how to calculate the
// input value for every given suggestion.
const getSuggestionValue = suggestion => suggestion.name;

// Use your imagination to render suggestions.
const renderSuggestion = suggestion => (
  <div>
    {suggestion.name}
  </div>
);

class Search extends React.Component {
  constructor() {
    super();

    // Autosuggest is a controlled component.
    // This means that you need to provide an input value
    // and an onChange handler that updates this value (see below).
    // Suggestions also need to be provided to the Autosuggest,
    // and they are initially empty because the Autosuggest is closed.
    this.state = {
      value: '',
      suggestions: []
    };
  }

  onChange = (event, { newValue }) => {
    this.setState({
      value: newValue
    });
  };

  // Autosuggest will call this function every time you need to update suggestions.
  // You already implemented this logic above, so just use it.
  onSuggestionsFetchRequested = ({ value }) => {
    getSuggestions(this.props.client, value).then(suggestions => {
        this.setState({
            suggestions: suggestions
          });
    })

  };

  // Autosuggest will call this function every time you need to clear suggestions.
  onSuggestionsClearRequested = () => {
    this.setState({
      suggestions: []
    });
  };

  render() {
    const { value, suggestions } = this.state;

    // Autosuggest will pass through all these props to the input.
    const inputProps = {
      placeholder: 'Type a programming language',
      value,
      onChange: this.onChange
    };

    // Finally, render it!
    return (
      <Autosuggest
        suggestions={suggestions}
        onSuggestionsFetchRequested={this.onSuggestionsFetchRequested}
        onSuggestionsClearRequested={this.onSuggestionsClearRequested}
        getSuggestionValue={getSuggestionValue}
        renderSuggestion={renderSuggestion}
        inputProps={inputProps}
      />
    );
  }
}

const GET_SEARCHALL = gql`
query searchAll($searchText: String) {
    searchCharacters(search: $searchText) {
      nodes {
        id,
        name
      }
    },
    searchGuilds(search: $searchText) {
      nodes {
        id,
        name
      }
    },
  }
`

export default withApollo(Search);