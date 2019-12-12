import React from 'react';
import { Icon, Input, AutoComplete } from 'antd';
import { withApollo } from 'react-apollo';
import { withRouter } from 'react-router-dom';
import gql from "graphql-tag";
import { SeasonContext } from './SeasonContext';

const Option = AutoComplete.Option;
const OptGroup = AutoComplete.OptGroup;

function renderTitle(title) {
  return (
    <span>
      {title}
    </span>
  );
}

const createOptions = dataSource =>  {
    return dataSource.map(group => (
    <OptGroup
        key={group.title}
        label={renderTitle(group.title)}
    >
        {group.children.map(opt => (
        <Option key={opt.title} value={opt.title} character_id={opt.character_id} guild_id={opt.guild_id}>
            {opt.title}
        </Option>
        ))}
    </OptGroup>
    ))
};

class Search extends React.Component {
  static contextType = SeasonContext;
  constructor() {
      super();

      this.state= {
          value: '',
          suggestions: []
      }
  }
  onSearch = value => {
    const season = this.context;
    this.props.client.query({
      query: GET_SEARCHALL,
      variables: { season,  searchText: value }
    }).then(result => {
      var self = this;
      let suggestions = [];
      if (result.data) {
        var characterGroup = {
          title: "Characters",
          children: result.data.searchCharacters.nodes.map(node => {
            return {
              character_id: node.id,
              title: node.name,
              count: 1000
            }
          })
        }
        var guildGroup = {
          title: "Guilds",
          children: result.data.searchGuilds.nodes.map(node => {
            return {
              guild_id: node.id,
              title: node.name,
              count: 1000
            }
          })
        }
        suggestions = [characterGroup, guildGroup]
      }
      self.setState({
        suggestions
      });
    });
    this.setState({
      value
    });
  }
  onSelect = (value, option) => {
    const season = this.context;
    if (option.props.character_id) {
      this.props.history.push(`/${season}/character/${option.props.character_id}`);
    } else if (option.props.guild_id) {
      this.props.history.push(`/${season}/guild/${option.props.guild_id}`);
    }
    this.setState({
      value: ''
    })
  }
  render() {
      return (
          <div className="certain-category-search-wrapper" style={{ width: 250, float: "right" }}>
            <AutoComplete
              className="certain-category-search"
              dropdownClassName="certain-category-search-dropdown"
              dropdownMatchSelectWidth={false}
              dropdownStyle={{ width: 250 }}
              size="large"
              style={{ width: '100%' }}
              dataSource={createOptions(this.state.suggestions)}
              placeholder="Search..."
              optionLabelProp="value"
              value={this.state.value}
              onSearch={this.onSearch}
              onSelect={this.onSelect}
            >
              <Input suffix={<Icon type="search" className="certain-category-icon" />} />
            </AutoComplete>
          </div>
        );
  }
}

const GET_SEARCHALL = gql`
query searchAll($season: Int, $searchText: String) {
    searchCharacters(season: $season, search: $searchText, filter: {season: {equalTo: $season}}) {
      nodes {
        id,
        name
      }
    },
    searchGuilds(season: $season, search: $searchText, filter: {season: {equalTo: $season}}) {
      nodes {
        id,
        name
      }
    },
  }
`

export default withRouter(withApollo(Search));