import React, { Component } from "react"
import { Query } from "react-apollo"
import gql from "graphql-tag"

export default class GuildInfoQuery extends Component {
  render() {
    const { guildId, children } = this.props

    return (
      <Query query={GET_GUILDINFO} variables={{guildId}}>
        {result => children(result)}
      </Query>
    )
  }
}

const GET_GUILDINFO = gql`
query guildInfo($guildId: Int!) {
  guildById(id: $guildId) {
    id,
    name,
    killmailsByAttackerGuildId {
      totalCount
    },
    killmailsByVictimGuildId {
      totalCount
    }
  }
}
`;
